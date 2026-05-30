using LineItem.Migrations.Helpers;
using FluentMigrator;

namespace LineItem.Migrations;

[Migration(20260215144900)]
public class ExampleMigration : Migration
{
    public override void Up()
    {
        Execute.Script(this.GetUpScript("tbl_app_user.sql"));
    }

    public override void Down()
    {
        Execute.Script(this.GetDownScript("drop_all_tables.sql"));
    }
}
