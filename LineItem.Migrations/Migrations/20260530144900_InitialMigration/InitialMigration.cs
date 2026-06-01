using FluentMigrator;
using LineItem.Migrations.Helpers;

namespace LineItem.Migrations;

[Migration(20260530144900)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Script(this.GetUpScript("fn_set_updated_at.sql"));

        Execute.Script(this.GetUpScript("tbl_app_user.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_insert.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_retrieve_by_id.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_update.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_delete.sql"));
    }

    public override void Down()
    {
        Execute.Script(this.GetDownScript("drop_all.sql"));
    }
}