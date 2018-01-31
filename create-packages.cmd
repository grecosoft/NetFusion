msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.9.2
for /r ./src %%x in (*.2.9.2.nupkg) do copy "%%x" ..\..\_packages
for /r ./src %%x in (*.2.9.2.symbols.nupkg) do copy "%%x" ..\..\_packages