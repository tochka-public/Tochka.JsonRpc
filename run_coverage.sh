set -e
cd "$(dirname "$0")"

rm -rf coverage
dotnet test -p:CollectCoverage=true -p:CoverletOutput="../../../coverage/" -p:MergeWith="../../../coverage/coverage.json" -p:CoverletOutputFormat=\"json,opencover\" -p:ExcludeByAttribute="GeneratedCode" -maxcpucount:1 src
reportgenerator -reports:coverage/coverage.opencover.xml -targetdir:coverage/report

# see result in coverage/report/index.html
