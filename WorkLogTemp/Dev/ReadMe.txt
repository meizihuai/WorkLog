worklog
"CREATE TABLE Worklog(day varchar(50) not null,name varchar(50) not null,project Varchar(200) default '', workContent varchar(2000) default '',city varchar(50) default '',issue varchar(200) default '',modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '',corp varchar(50) default '',memo varchar(80) primary key,account varchar(20))" ', CONSTRAINT Worklog PRIMARY KEY(memo)--定义主键

logAccount

"CREATE TABLE LogAccount(account varchar(50) not null primary key,password Varchar(100) not null, name varchar(100) not null,corp varchar(50) default '',state varchar(20))" 

logProject
"CREATE TABLE logProject(project varchar(50) not null primary key,corp varchar(50) default '',state varchar(20))" 


weeklog    'day必须是每周5
"CREATE TABLE weeklog(day varchar(50) not null,name varchar(50) not null,account varchar(20) not null,project Varchar(200) default '', workSum varchar(2000) default '', problem varchar(2000) default '', plan varchar(2000) default '', advise varchar(2000) default '',city varchar(50) default '',issue varchar(200) default '',corp varchar(50) default '',memo varchar(80) primary key,case1 blob, case2 blob,case3 blob,modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '')


create table dyml01_new as select DAY,ACCOUNT,NAME,PROJECT,WORKCONTENT,ISSUE,CITY,CORP,MEMO,MODIFIEDBY,MODIFIEDDATE	 from Worklog ;

drop table Worklog ;

alter table dyml01_new  rename to Worklog


alter table QOE_VIDEO_TABLE modify (BDLON NUMBER(10,6) )