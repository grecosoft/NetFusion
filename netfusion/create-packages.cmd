msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=4.0.12
for /r ./src %%x in (*.4.0.12.nupkg) do copy "%%x" ..\..\_packages
for /r ./src %%x in (*.4.0.12.symbols.nupkg) do copy "%%x" ..\..\_packages