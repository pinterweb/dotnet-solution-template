if not exists (select * from master.sys.databases where name = N'BusinessApp')
begin
    create database BusinessApp;
    alter database BusinessApp set recovery simple;
end
