﻿// Copyright © 2013 Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Config;
using MySql.Data.MySqlClient;
#if EF6
using System.Data.Common;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.History;
#endif

namespace MySql.Data.Entity.CodeFirst.Tests
{
  public class MovieCBC
  {
    public int ID { get; set; }
    public string Title { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Genre { get; set; }
    public decimal Price { get; set; }
  }

  [DbConfigurationType(typeof(MyConfiguration))]
  public class MovieCodedBasedConfigDBContext : DbContext
  {
    public DbSet<MovieCBC> Movies { get; set; }

    public MovieCodedBasedConfigDBContext()
    {
      Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MovieCBCDBInitialize<MovieCodedBasedConfigDBContext>());
#if EF6
      Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MigrateDatabaseToLatestVersion<MovieCodedBasedConfigDBContext, Configuration>());
#endif
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<MovieCBC>().Property(x => x.Price).HasPrecision(16, 2);
#if EF6
      //TODO: THE CURRENT CLASS FROM EF THAT EXPOSES THE METHOD "modelBuilder.Entity<TEntity>().MapToStoredProcedures()" HAS NO STATIC OR VIRTUAL MEMBERS FOR IMPLEMENTATION
      //      NEED TO VERIFY IS THERE ANY OTHER WAY TO GENERATE THE STORED PROCEDURES, THE MAP TO A STORED PROCEDURE IS AUTOMATIC BUT NOT THE GENERATION AND CONFIGURATION
      //modelBuilder.Entity<MovieCBC>().MapToStoredProcedures();
#endif
    }
  }

  public class MovieCBCDBInitialize<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
  {
    public void InitializeDatabase(TContext context)
    {
      context.Database.Delete();
      context.Database.CreateIfNotExists();
      this.Seed(context);
      context.SaveChanges();
    }

    protected virtual void Seed(TContext context)
    {
    }
  }

#if EF6
  public class MyHistoryContext : MySqlHistoryContext
  {
    public MyHistoryContext(DbConnection existingConnection, string defaultSchema)
      : base(existingConnection, defaultSchema)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<HistoryRow>().ToTable("__MySqlMigrations");
      modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasColumnName("_MigrationId");
      modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasColumnName("_ContextKey");
      modelBuilder.Entity<HistoryRow>().Property(h => h.Model).HasColumnName("_Model");
      modelBuilder.Entity<HistoryRow>().Property(h => h.ProductVersion).HasColumnName("_ProductVersion");
    }
  }

  public class Configuration : DbMigrationsConfiguration<MovieCodedBasedConfigDBContext>
  {
    public Configuration()
    {
      CodeGenerator = new MySqlMigrationCodeGenerator();
      AutomaticMigrationsEnabled = true;
      SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
      HistoryContextFactory = (existingConnection, defaultSchema) => new MyHistoryContext(existingConnection, defaultSchema);
    }
  }
#endif
}