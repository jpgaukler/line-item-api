CREATE TABLE lineitem.product
(
  id                  BIGINT       NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  product_category_id BIGINT       NOT NULL REFERENCES lineitem.product_category (id),
  name                VARCHAR(100) NOT NULL,
  description         VARCHAR(500) NOT NULL,
  search_vector       TSVECTOR     NOT NULL GENERATED ALWAYS AS (TO_TSVECTOR('english', name || ' ' || description)) STORED,
  active_version      INT          NULL, -- null on creation until product_version record is inserted
  created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  created_by          BIGINT       NOT NULL REFERENCES lineitem.app_user (id),
  updated_at          TIMESTAMPTZ  NULL,
  updated_by          BIGINT       NULL REFERENCES lineitem.app_user (id),

  FOREIGN KEY (id, active_version)
    REFERENCES lineitem.product_version (product_id, version)
);

CREATE TRIGGER trg_product_updated_at
  BEFORE UPDATE
  ON lineitem.product
  FOR EACH ROW
EXECUTE FUNCTION lineitem.set_updated_at();

-- Filter by product category
CREATE INDEX idx_product_product_category_id ON lineitem.product (product_category_id);

-- Text search on product name/description (for search bars)
CREATE INDEX idx_product_text_search ON lineitem.product USING GIN (search_vector);