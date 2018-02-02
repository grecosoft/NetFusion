msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.9.19
for /r ./src %%x in (*.2.9.19.nupkg) do copy "%%x" ..\..\_packages
for /r ./src %%x in (*.2.9.19.symbols.nupkg) do copy "%%x" ..\..\_packages