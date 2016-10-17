using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using CVaS.DAL;

namespace CVaS.DAL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20161017110505_AddedPropertiesToAlgo")]
    partial class AddedPropertiesToAlgo
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CVaS.DAL.Model.Algorithm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CodeName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 64);

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Algorithms");
                });
        }
    }
}
