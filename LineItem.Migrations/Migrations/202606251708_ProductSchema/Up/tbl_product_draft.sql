CREATE TABLE lineitem.product_draft
(
  id                  BIGINT      NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  base_product_id     BIGINT      NULL,
  base_version        INT         NULL,
  product_category_id BIGINT      NOT NULL REFERENCES lineitem.product_category (id),
  product_data_json   JSONB       NOT NULL,
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_by          BIGINT      NOT NULL REFERENCES lineitem.app_user (id),
  updated_at          TIMESTAMPTZ NULL,
  updated_by          BIGINT      NULL REFERENCES lineitem.app_user (id),

  -- allows multiple "new product" drafts but enforces one draft per existing product
  UNIQUE (base_product_id),

  -- which version this draft branched from
  FOREIGN KEY (base_product_id, base_version)
    REFERENCES lineitem.product_version (product_id, version)
);

CREATE TRIGGER trg_product_draft_updated_at
  BEFORE UPDATE
  ON lineitem.product_draft
  FOR EACH ROW
EXECUTE FUNCTION lineitem.set_updated_at();

-- Filter by product category
CREATE INDEX idx_product_draft_product_category_id ON lineitem.product_draft (product_category_id);
