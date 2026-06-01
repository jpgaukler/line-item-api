CREATE TABLE lineitem.app_user
(
  id           BIGINT       NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  external_id  VARCHAR      NOT NULL UNIQUE, -- ID from identity provider
  display_name VARCHAR(100) NOT NULL,
  created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at   TIMESTAMPTZ  NULL
);

CREATE TRIGGER trg_app_user_updated_at
  BEFORE UPDATE
  ON lineitem.app_user
  FOR EACH ROW
EXECUTE FUNCTION lineitem.set_updated_at();

CREATE INDEX idx_app_user_external_id ON lineitem.app_user (external_id);

