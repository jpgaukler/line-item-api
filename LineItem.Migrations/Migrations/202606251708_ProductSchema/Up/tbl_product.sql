CREATE TABLE lineitem.product_category
(
  id         BIGINT       NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  name       VARCHAR(100) NOT NULL,
  created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  created_by BIGINT       NOT NULL,
  updated_at TIMESTAMPTZ  NULL,
  updated_by BIGINT       NULL
);

CREATE TABLE lineitem.product
(
  id                  BIGINT       NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  product_category_id INT          NOT NULL REFERENCES lineitem.product_category (id),
  name                VARCHAR(100) NOT NULL,
  description         VARCHAR(500) NULL,
  active_version      INT          NULL, -- null until first version is published
  created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  created_by          BIGINT       NOT NULL,
  updated_at          TIMESTAMPTZ  NULL,
  updated_by          BIGINT       NULL,

  FOREIGN KEY (id, active_version)
    REFERENCES lineitem.product_version (product_id, version)
);

CREATE TRIGGER trg_product_updated_at
  BEFORE UPDATE
  ON lineitem.product
  FOR EACH ROW
EXECUTE FUNCTION lineitem.set_updated_at();



CREATE TABLE lineitem.product_version
(
  product_id   INT         NOT NULL REFERENCES lineitem.product (id),
  version      INT         NOT NULL,
  product_data JSONB       NOT NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_by   BIGINT      NOT NULL,

  PRIMARY KEY (product_id, version)
);

CREATE TABLE lineitem.product_draft
(
  product_id   INT         NOT NULL REFERENCES lineitem.product (id),
  base_version INT         NULL, -- which published version this draft branched from
  product_data JSONB       NOT NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_by   INT         NOT NULL,
  updated_at   TIMESTAMPTZ NULL,
  updated_by   BIGINT      NULL,

  PRIMARY KEY (product_id)       -- enforces only one draft per product
);