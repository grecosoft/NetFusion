msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.1.5
for /r ./src %%x in (*.2.1.5.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.1.5.symbols.nupkg) do copy "%%x" ..\_packages