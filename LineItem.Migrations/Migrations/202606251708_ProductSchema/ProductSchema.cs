// ReSharper disable CheckNamespace

using FluentMigrator;
using LineItem.Migrations.Helpers;

namespace LineItem.Migrations;

[Migration(202606251708)]
public class ProductSchema : Migration
{
    public override void Up()
    {
        Execute.Script(this.GetUpScript("tbl_product.sql"));
    }

    public override void Down()
    {
        Execute.Script("DROP TABLE lineitem.product");
    }
}