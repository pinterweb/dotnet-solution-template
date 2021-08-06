﻿// <auto-generated />
using System;
using BusinessApp.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BusinessApp.Test.Shared.Migrations
{
    [DbContext(typeof(BusinessAppTestDbContext))]
    [Migration("20210318173620_CreateTestDb")]
    partial class CreateTestDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BusinessApp.Infrastructure.EventMetadata<BusinessApp.WebApi.Delete+WebDomainEvent>", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("EventMetadataId");

                    b.Property<long>("CausationId")
                        .HasColumnType("bigint")
                        .HasColumnName("CausationId");

                    b.Property<long>("CorrelationId")
                        .HasColumnType("bigint")
                        .HasColumnName("CorrelationId");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("varchar(500)");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset(0)");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId")
                        .IsUnique();

                    b.ToTable("DeleteEvent", "evt");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.Metadata", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("MetadataId");

                    b.Property<string>("DataSetName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("DataSetName");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset(0)")
                        .HasColumnName("OccurredUtc");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("TypeName");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("Username");

                    b.HasKey("Id");

                    b.ToTable("Metadata", "dbo");

                    b.HasDiscriminator<string>("DataSetName").HasValue("Metadata");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.RequestMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("RequestMetadataId")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EventTriggers")
                        .IsRequired()
                        .HasColumnType("varchar(max)");

                    b.Property<string>("RequestType")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("ResponseType")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("RequestMetadata", "dbo");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.AggregateRootStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("AggregateRootStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ChildResponseStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ResponseStubId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ResponseStubId");

                    b.ToTable("ChildResponseStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.DomainEventStub", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("DomainEventStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.RequestStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("RequestStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ResponseStubId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ResponseStubId");

                    b.ToTable("ResponseStub");
                });

            modelBuilder.Entity("BusinessApp.WebApi.Delete+Query", b =>
                {
                    b.Property<int>("DeleteQueryRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Id")
                        .HasColumnType("int")
                        .HasColumnName("DeleteQueryId");

                    b.Property<long>("MetadataId")
                        .HasColumnType("bigint");

                    b.HasKey("DeleteQueryRequestId");

                    b.HasIndex("MetadataId")
                        .IsUnique();

                    b.ToTable("DeleteQuery");
                });

            modelBuilder.Entity("BusinessApp.WebApi.PostOrPut+Body", b =>
                {
                    b.Property<int>("PostOrPutBodyRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Id")
                        .HasColumnType("int")
                        .HasColumnName("PostOrPutId");

                    b.Property<long>("LongerId")
                        .HasColumnType("bigint");

                    b.Property<long>("MetadataId")
                        .HasColumnType("bigint");

                    b.HasKey("PostOrPutBodyRequestId");

                    b.HasIndex("MetadataId")
                        .IsUnique();

                    b.ToTable("PostOrPutBody");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.Delete+Query>", b =>
                {
                    b.HasBaseType("BusinessApp.Infrastructure.Metadata");

                    b.HasDiscriminator().HasValue("Metadata<Query>");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.PostOrPut+Body>", b =>
                {
                    b.HasBaseType("BusinessApp.Infrastructure.Metadata");

                    b.HasDiscriminator().HasValue("Metadata<Body>");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.EventMetadata<BusinessApp.WebApi.Delete+WebDomainEvent>", b =>
                {
                    b.HasOne("BusinessApp.Infrastructure.Metadata", null)
                        .WithOne()
                        .HasForeignKey("BusinessApp.Infrastructure.EventMetadata<BusinessApp.WebApi.Delete+WebDomainEvent>", "CorrelationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("BusinessApp.WebApi.Delete+WebDomainEvent", "Event", b1 =>
                        {
                            b1.Property<long>("EventMetadata<WebDomainEvent>Id")
                                .HasColumnType("bigint");

                            b1.Property<int?>("Id")
                                .HasColumnType("int");

                            b1.HasKey("EventMetadata<WebDomainEvent>Id");

                            b1.ToTable("DeleteEvent");

                            b1.WithOwner()
                                .HasForeignKey("EventMetadata<WebDomainEvent>Id");
                        });

                    b.Navigation("Event")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ChildResponseStub", b =>
                {
                    b.HasOne("BusinessApp.Test.Shared.ResponseStub", "ResponseStub")
                        .WithMany()
                        .HasForeignKey("ResponseStubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ResponseStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.HasOne("BusinessApp.Test.Shared.ResponseStub", null)
                        .WithMany("ChildResponseStubs")
                        .HasForeignKey("ResponseStubId");
                });

            modelBuilder.Entity("BusinessApp.WebApi.Delete+Query", b =>
                {
                    b.HasOne("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.Delete+Query>", null)
                        .WithOne("Data")
                        .HasForeignKey("BusinessApp.WebApi.Delete+Query", "MetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessApp.WebApi.PostOrPut+Body", b =>
                {
                    b.HasOne("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.PostOrPut+Body>", null)
                        .WithOne("Data")
                        .HasForeignKey("BusinessApp.WebApi.PostOrPut+Body", "MetadataId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.Navigation("ChildResponseStubs");
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.Delete+Query>", b =>
                {
                    b.Navigation("Data")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessApp.Infrastructure.Metadata<BusinessApp.WebApi.PostOrPut+Body>", b =>
                {
                    b.Navigation("Data")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
