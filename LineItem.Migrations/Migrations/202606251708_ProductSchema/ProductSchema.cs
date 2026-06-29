// ReSharper disable CheckNamespace

using FluentMigrator;
using LineItem.Migrations.Helpers;

namespace LineItem.Migrations;

[Migration(202606251708)]
public class ProductSchema : Migration
{
    public override void Up()
    {
        Execute.Script(this.GetUpScript("tbl_product_category.sql"));
        Execute.Script(this.GetUpScript("tbl_product.sql"));
        Execute.Script(this.GetUpScript("tbl_product_version.sql"));
        Execute.Script(this.GetUpScript("tbl_product_draft.sql"));

        Execute.Script(this.GetUpScript("fk_product_active_version.sql"));

        Execute.Script(this.GetUpScript("fn_product_draft_insert.sql"));
        Execute.Script(this.GetUpScript("fn_product_draft_retrieve_by_id.sql"));
        Execute.Script(this.GetUpScript("fn_product_draft_update.sql"));
        Execute.Script(this.GetUpScript("fn_product_draft_delete.sql"));
        Execute.Script(this.GetUpScript("fn_product_insert.sql"));
        Execute.Script(this.GetUpScript("fn_product_retrieve_active_version_by_id.sql"));
        Execute.Script(this.GetUpScript("fn_product_retrieve_specific_version_by_id.sql"));
        Execute.Script(this.GetUpScript("fn_product_retrieve_by_category_id.sql"));
        Execute.Script(this.GetUpScript("fn_product_search.sql"));
        Execute.Script(this.GetUpScript("fn_product_update_active_version.sql"));
        Execute.Script(this.GetUpScript("fn_product_version_insert.sql"));
    }

    public override void Down()
    {
        Execute.Sql("DROP FUNCTION lineitem.product_draft_insert(BIGINT, INT, JSONB, BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_draft_retrieve_by_id(BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_draft_update(BIGINT, JSONB, BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_draft_delete(BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_insert(BIGINT, VARCHAR, VARCHAR, BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_retrieve_active_version_by_id(BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_retrieve_specific_version_by_id(BIGINT, INT)");
        Execute.Sql("DROP FUNCTION lineitem.product_retrieve_by_category_id(BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_search(VARCHAR)");
        Execute.Sql("DROP FUNCTION lineitem.product_update_active_version(BIGINT, INT, BIGINT)");
        Execute.Sql("DROP FUNCTION lineitem.product_version_insert(BIGINT, JSONB, BIGINT)");

        Execute.Sql("ALTER TABLE lineitem.product DROP CONSTRAINT fk_product_active_version;");

        Execute.Sql("DROP TABLE lineitem.product_draft");
        Execute.Sql("DROP TABLE lineitem.product_version");
        Execute.Sql("DROP TABLE lineitem.product");
        Execute.Sql("DROP TABLE lineitem.product_category");
    }
}