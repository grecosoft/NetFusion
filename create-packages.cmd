msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.1.4
for /r ./src %%x in (*.2.1.4.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.1.4.symbols.nupkg) do copy "%%x" ..\_packages