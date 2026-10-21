"""Verify the actual ZIP, its hashes and self-contained startup from a path with spaces."""
from pathlib import Path
import hashlib
import json
import os
import subprocess
import zipfile
import argparse

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--version',default='0.1.1')
args=parser.parse_args()

root=Path(__file__).resolve().parents[1]
archive=root/f'artifacts/ScreenHunter-{args.version}-win-x64.zip'
target=root/'artifacts/Prueba ZIP portable'
target.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(archive) as z:
    for info in z.infolist():
        resolved=(target/info.filename).resolve()
        if not resolved.is_relative_to(target.resolve()): raise SystemExit('Unsafe ZIP path')
    z.extractall(target)
package=target/f'ScreenHunter-{args.version}-win-x64'
manifest=json.loads((package/'SHA256SUMS.json').read_text(encoding='utf-8'))
for name,expected in manifest.items():
    path=(package/name).resolve()
    if not path.is_relative_to(package.resolve()): raise SystemExit('Unsafe manifest path')
    if hashlib.sha256(path.read_bytes()).hexdigest()!=expected: raise SystemExit('Hash mismatch: '+name)
config=json.loads((package/'ScreenHunter.runtimeconfig.json').read_text(encoding='utf-8'))['runtimeOptions']
if 'frameworks' in config or 'framework' in config or 'includedFrameworks' not in config:
    raise SystemExit('Runtime config is not self-contained')
env=os.environ.copy()
for key in ('DOTNET_ROOT','DOTNET_ROOT_X64'):env[key]=str(root/'artifacts/No runtime installed')
env['DOTNET_MULTILEVEL_LOOKUP']='0'
env['COREHOST_TRACE']='1'
trace_path=root/f'artifacts/zip-host-trace-{args.version}.txt'
env['COREHOST_TRACEFILE']=str(trace_path)
report_dir=root/f'artifacts/zip-verification-{args.version}'
result=subprocess.run([str(package/'ScreenHunter.exe'),'--self-test','--output',str(report_dir)],env=env,cwd=package,timeout=60)
report=json.loads((report_dir/'report.json').read_text(encoding='utf-8'))
if result.returncode or report['status']!='passed':raise SystemExit('Packaged executable self-test failed')
trace=trace_path.read_text(encoding='utf-8',errors='replace').lower()
for name in ('hostfxr.dll','hostpolicy.dll','coreclr.dll'):
    if str(package/name).lower() not in trace:raise SystemExit('Local runtime not proven: '+name)
summary={'status':'passed','zip':str(archive),'zipBytes':archive.stat().st_size,'sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'verifiedFiles':len(manifest),'checks':len(report['checks']),'runtimeResolution':'local package','pathWithSpaces':True,'cleanMachineTest':False}
(root/f'artifacts/package-verification-{args.version}.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary,indent=2))
