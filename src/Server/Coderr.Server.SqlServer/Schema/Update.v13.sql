﻿create table ErrorReportSpikes
(
    Id int identity not null primary key,
    ApplicationId int not null constraint FK_ErrorReportSpikes_Applications REFERENCES Applications(Id) ON DELETE CASCADE,
    SpikeDate datetime not null,
    Count int not null,
    NotifiedAccounts varchar(max) not null
);

UPDATE DatabaseSchema SET Version = 13;