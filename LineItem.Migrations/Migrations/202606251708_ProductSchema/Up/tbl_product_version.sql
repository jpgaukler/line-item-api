CREATE TABLE lineitem.product_version
(
  product_id   BIGINT      NOT NULL REFERENCES lineitem.product (id),
  version      INT         NOT NULL,
  product_data JSONB       NOT NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_by   BIGINT      NOT NULL REFERENCES lineitem.app_user (id),

  PRIMARY KEY (product_id, version)
);
