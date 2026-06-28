CREATE TABLE lineitem.product_category
(
  id         BIGINT       NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  name       VARCHAR(100) NOT NULL,
  created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  created_by BIGINT       NOT NULL REFERENCES lineitem.app_user (id),
  updated_at TIMESTAMPTZ  NULL,
  updated_by BIGINT       NULL REFERENCES lineitem.app_user (id)
);

CREATE TRIGGER trg_product_category_updated_at
  BEFORE UPDATE
  ON lineitem.product_category
  FOR EACH ROW
EXECUTE FUNCTION lineitem.set_updated_at();
