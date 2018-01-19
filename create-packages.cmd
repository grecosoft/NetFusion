msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=3.1.0
for /r ./src %%x in (*.3.1.0.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.3.1.0.symbols.nupkg) do copy "%%x" ..\_packages