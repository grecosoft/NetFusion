msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=3.9.1
for /r ./src %%x in (*.3.9.1.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.3.9.1.symbols.nupkg) do copy "%%x" ..\_packages