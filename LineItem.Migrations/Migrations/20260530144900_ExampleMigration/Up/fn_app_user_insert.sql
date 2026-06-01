CREATE OR REPLACE FUNCTION lineitem.app_user_insert(
  p_external_id VARCHAR,
  p_display_name VARCHAR
)
  RETURNS BIGINT
AS
$$
DECLARE
  new_id BIGINT;
BEGIN
  INSERT INTO lineitem.app_user (external_id, display_name)
  VALUES (p_external_id, p_display_name)
  RETURNING id INTO new_id;

  RETURN new_id;
END;
$$ LANGUAGE plpgsql;