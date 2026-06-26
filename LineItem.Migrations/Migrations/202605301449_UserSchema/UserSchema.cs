// ReSharper disable CheckNamespace

using FluentMigrator;
using LineItem.Migrations.Helpers;

namespace LineItem.Migrations;

[Migration(202605301449)]
public class UserSchema : Migration
{
    public override void Up()
    {
        Execute.Script(this.GetUpScript("fn_set_updated_at.sql"));
        Execute.Script(this.GetUpScript("tbl_app_user.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_insert.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_retrieve_by_id.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_retrieve_by_external_id.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_update.sql"));
        Execute.Script(this.GetUpScript("fn_app_user_delete.sql"));
    }

    public override void Down()
    {
        Execute.Script("DROP FUNCTION lineitem.app_user_insert(VARCHAR, VARCHAR)");
        Execute.Script("DROP FUNCTION lineitem.app_user_retrieve_by_id(BIGINT)");
        Execute.Script("DROP FUNCTION lineitem.app_user_retrieve_by_external_id(VARCHAR)");
        Execute.Script("DROP FUNCTION lineitem.app_user_update(BIGINT, VARCHAR, VARCHAR)");
        Execute.Script("DROP FUNCTION lineitem.app_user_delete(BIGINT)");
        Execute.Script("DROP TABLE lineitem.app_user");
        Execute.Script("DROP FUNCTION lineitem.set_updated_at()");
    }
}