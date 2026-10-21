"""Build an offline self-contained x64 folder from the installed Microsoft runtime.

No NuGet packages, downloads or machine-wide writes. Uses the official SDK apphost,
runtime assets and dependency manifests; licenses/notices travel with the bundle.
"""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--version', default='0.1.1')
parser.add_argument('--build-output', help='Use an isolated output folder when the normal build is running')
args = parser.parse_args()
runtime_root = Path('C:/Program Files/dotnet')
version = '10.0.12'
core = runtime_root / 'shared/Microsoft.NETCore.App' / version
desktop = runtime_root / 'shared/Microsoft.WindowsDesktop.App' / version
hostfxr = runtime_root / 'host/fxr' / version / 'hostfxr.dll'
for required in (core, desktop, hostfxr):
    if not required.exists():
        raise SystemExit(f'Required installed runtime is missing: {required}')
project = ROOT / 'src/ScreenHunter.Presentation.WinForms'
build = Path(args.build_output).resolve() if args.build_output else project / 'bin/Release/net10.0-windows'
subprocess.run(['dotnet', 'build', str(project), '-c', 'Release', '--nologo', '--output', str(build)], cwd=ROOT, check=True)
out = ROOT / 'artifacts' / f'ScreenHunter-{args.version}-win-x64'
out.mkdir(parents=True, exist_ok=True)
# Reusing a version directory must not include a user's settings or projects.
if (out/'settings.json').exists() or list(out.glob('*.shproj')):
    raise SystemExit('Output contains user data; choose a new version or a clean output directory.')
for directory in (core, desktop, build):
    for source in directory.rglob('*'):
        if source.is_file() and source.suffix.lower() != '.pdb':
            dest = out / source.relative_to(directory)
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, dest)
shutil.copy2(hostfxr, out/'hostfxr.dll')
license_dir=out/'licenses'
license_dir.mkdir(exist_ok=True)
for name in ('LICENSE.txt','ThirdPartyNotices.txt'):
    shutil.copy2(runtime_root/name,license_dir/name)
app_deps = json.loads((build/'ScreenHunter.deps.json').read_text(encoding='utf-8'))
target_name = f'.NETCoreApp,Version=v10.0/win-x64'
merged_target, libraries = {}, {}
for path in (core/'Microsoft.NETCore.App.deps.json', desktop/'Microsoft.WindowsDesktop.App.deps.json', build/'ScreenHunter.deps.json'):
    data = json.loads(path.read_text(encoding='utf-8-sig'))
    targets = data['targets']
    name = data['runtimeTarget']['name']
    merged_target.update(targets[name])
    libraries.update(data['libraries'])
app_deps['runtimeTarget'] = {'name': target_name, 'signature': ''}
app_deps['targets'] = {'.NETCoreApp,Version=v10.0': {}, target_name: merged_target}
app_deps['libraries'] = libraries
(out/'ScreenHunter.deps.json').write_text(json.dumps(app_deps, indent=2), encoding='utf-8')
config = json.loads((build/'ScreenHunter.runtimeconfig.json').read_text(encoding='utf-8'))
options = config['runtimeOptions']
frameworks = options.pop('frameworks', None)
if frameworks is None:
    frameworks = [options.pop('framework')]
options['includedFrameworks'] = [{'name': f['name'], 'version': version} for f in frameworks]
(out/'ScreenHunter.runtimeconfig.json').write_text(json.dumps(config, indent=2), encoding='utf-8')
shutil.copytree(ROOT/'docs', out/'docs', dirs_exist_ok=True)
example=ROOT/'artifacts/final-verification/sample.shproj'
if example.exists():
    (out/'examples').mkdir(exist_ok=True)
    shutil.copy2(example,out/'examples/sample.shproj')
for name in ('LEEME.txt', 'THIRD-PARTY-NOTICES.txt'):
    source=ROOT/name
    if source.exists(): shutil.copy2(source,out/name)
manifest={str(p.relative_to(out)).replace('\\','/'): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(out.rglob('*')) if p.is_file() and p.name != 'SHA256SUMS.json'}
(out/'SHA256SUMS.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
archive=Path(str(out)+'.zip')
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for file in sorted(out.rglob('*')):
        if file.is_file(): z.write(file,Path(out.name)/file.relative_to(out))
print(json.dumps({'folder':str(out),'zip':str(archive),'bytes':archive.stat().st_size,'runtime':version},indent=2))
