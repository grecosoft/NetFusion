msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.1.6
for /r ./src %%x in (*.2.1.6.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.1.6.symbols.nupkg) do copy "%%x" ..\_packages