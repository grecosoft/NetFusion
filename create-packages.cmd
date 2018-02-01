msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.9.15
for /r ./src %%x in (*.2.9.15.nupkg) do copy "%%x" ..\..\_packages
for /r ./src %%x in (*.2.9.15.symbols.nupkg) do copy "%%x" ..\..\_packages