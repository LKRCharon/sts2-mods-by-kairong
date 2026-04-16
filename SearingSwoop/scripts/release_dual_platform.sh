#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_FILE="${PROJECT_DIR}/SearingSwoop.csproj"
MOD_ID="SearingSwoop"
MOD_FOLDER_NAME="SearingSwoop"
DOTNET_BIN="${PROJECT_DIR}/../.dotnet/dotnet"

if [[ ! -x "${DOTNET_BIN}" ]]; then
  DOTNET_BIN="dotnet"
fi

if ! command -v "${DOTNET_BIN}" >/dev/null 2>&1; then
  echo "dotnet not found. install dotnet or repo-local .dotnet first." >&2
  exit 1
fi

VERSION="$(rg -oN '"version"\s*:\s*"[^"]+"' "${PROJECT_DIR}/${MOD_ID}.json" | sed -E 's/.*"([^"]+)"/\1/' | head -n 1)"
if [[ -z "${VERSION}" ]]; then
  VERSION="dev"
fi

BUILD_CONFIG="${1:-Release}"
DIST_ROOT="${PROJECT_DIR}/dist/${VERSION}"
STAGE_ROOT="${DIST_ROOT}/stage"
MODS_SINK="${STAGE_ROOT}/_mods_sink/"
BASELIB_MATRIX="${BASELIB_MATRIX:-3.0.3 3.0.5}"
# shellcheck disable=SC2206
BASELIB_VERSIONS=(${BASELIB_MATRIX})
PLATFORMS=(mac win)
RELEASE_FILES=()

DLL_PATH="${PROJECT_DIR}/.godot/mono/temp/bin/${BUILD_CONFIG}/${MOD_ID}.dll"
PCK_PATH="${PROJECT_DIR}/.godot/mono/temp/bin/${BUILD_CONFIG}/${MOD_ID}.pck"
MANIFEST_PATH="${PROJECT_DIR}/${MOD_ID}.json"
ASSET_DIR="${PROJECT_DIR}/${MOD_FOLDER_NAME}"

if [[ ! -f "${MANIFEST_PATH}" ]]; then
  echo "missing manifest: ${MANIFEST_PATH}" >&2
  exit 1
fi

if [[ ! -d "${ASSET_DIR}" ]]; then
  echo "missing asset folder: ${ASSET_DIR}" >&2
  exit 1
fi

mkdir -p "${STAGE_ROOT}"
find "${DIST_ROOT}" -maxdepth 1 -type f -name "${MOD_ID}-${VERSION}-*.zip" -delete 2>/dev/null || true

build_one() {
  local baselib_version="$1"
  echo "[build] ${MOD_ID} (${BUILD_CONFIG}) with BaseLib ${baselib_version}"
  DOTNET_ROLL_FORWARD=Major "${DOTNET_BIN}" build "${PROJECT_FILE}" -c "${BUILD_CONFIG}" \
    "/p:ModsPath=${MODS_SINK}/${baselib_version}/" \
    "/p:BaseLibVersion=${baselib_version}"
}

package_one() {
  local baselib_version="$1"
  local platform="$2"
  local stage_dir="${STAGE_ROOT}/${MOD_ID}-baselib-${baselib_version}-${platform}/${MOD_ID}"
  local zip_path="${DIST_ROOT}/${MOD_ID}-${VERSION}-baselib-${baselib_version}-${platform}.zip"

  if [[ ! -f "${DLL_PATH}" ]]; then
    echo "missing build output: ${DLL_PATH}" >&2
    exit 1
  fi

  mkdir -p "${stage_dir}"
  cp "${DLL_PATH}" "${stage_dir}/"
  cp "${MANIFEST_PATH}" "${stage_dir}/"
  cp -R "${ASSET_DIR}" "${stage_dir}/"

  if [[ -f "${PCK_PATH}" ]]; then
    cp "${PCK_PATH}" "${stage_dir}/"
  fi

  (
    cd "${STAGE_ROOT}/${MOD_ID}-baselib-${baselib_version}-${platform}"
    zip -qry "${zip_path}" "${MOD_ID}"
  )

  echo "packaged: ${zip_path}"
  RELEASE_FILES+=("${zip_path}")
}

echo "[1/4] Building and packaging matrix"
for baselib_version in "${BASELIB_VERSIONS[@]}"; do
  build_one "${baselib_version}"
  for platform in "${PLATFORMS[@]}"; do
    package_one "${baselib_version}" "${platform}"
  done
done

echo "[4/4] Done"
echo "Release bundles:"
for bundle in "${RELEASE_FILES[@]}"; do
  echo "  - ${bundle}"
done
