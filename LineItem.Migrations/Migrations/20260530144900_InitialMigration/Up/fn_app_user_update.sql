CREATE OR REPLACE FUNCTION lineitem.app_user_update(
  p_external_id TEXT,
  p_display_name TEXT
)
  RETURNS BOOLEAN
AS
$$
DECLARE
  rows_updated INTEGER;
BEGIN
  UPDATE lineitem.app_user
  SET display_name = p_display_name
  WHERE external_id = p_external_id;

  GET DIAGNOSTICS rows_updated = ROW_COUNT;

  RETURN rows_updated > 0;
END;
$$ LANGUAGE plpgsql;
