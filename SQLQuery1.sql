
CREATE TABLE [dbo].[Applications] (
    [Id]          INT IDENTITY(1,1)          NOT NULL,
    [name]        VARCHAR (50) NOT NULL UNIQUE,
    [creation_dt] DATETIME     NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Containers] (
    [Id]          INT IDENTITY(1,1)          NOT NULL,
    [name]        VARCHAR (50) NOT NULL UNIQUE,
    [creation_dt] DATETIME     NOT NULL,
    [parent]      INT          NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Containers_ToApplications] FOREIGN KEY ([parent]) REFERENCES [dbo].[Applications] ([Id])
);


CREATE TABLE [dbo].[Data] (
    [Id]          INT IDENTITY(1,1)          NOT NULL,
    [name]        VARCHAR (50) NOT NULL UNIQUE,
    [content]     VARCHAR (50) NOT NULL,
    [creation_dt] DATETIME     NOT NULL,
    [parent]      INT          NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Data_ToContainers] FOREIGN KEY ([parent]) REFERENCES [dbo].[Containers] ([Id])
);


CREATE TABLE [dbo].[Subscriptions] (
    [Id]          INT IDENTITY(1,1)          NOT NULL,
    [name]        VARCHAR (50) NOT NULL UNIQUE,
    [creation_dt] DATETIME     NOT NULL,
    [parent]      INT          NOT NULL,
    [event]       INT           NOT NULL CHECK (event IN (1, 2)),
    [endpoint]    VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Subscriptions_ToContainers] FOREIGN KEY ([parent]) REFERENCES [dbo].[Containers] ([Id])
);