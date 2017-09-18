msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.76
for /r ./src %%x in (*.2.0.76.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.0.76.symbols.nupkg) do copy "%%x" ..\_packages