using FluentMigrator.Runner.VersionTableInfo;

namespace LineItem.Migrations.Metadata;

[VersionTableMetaData]
public class VersionTableMetadata : IVersionTableMetaData
{
    public virtual string SchemaName => "myapp"; // replace with your app name
    public virtual string TableName => "migration_version_history";
    public virtual string ColumnName => "migration";
    public virtual string UniqueIndexName => "idx_migration_version_history";
    public virtual string AppliedOnColumnName => "applied_on";
    public virtual string DescriptionColumnName => "description";
    public virtual bool OwnsSchema => true;
    public virtual bool CreateWithPrimaryKey => false;
}
