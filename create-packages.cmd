msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.1.10
for /r ./src %%x in (*.2.1.10.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.1.10.symbols.nupkg) do copy "%%x" ..\_packages