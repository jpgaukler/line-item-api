CREATE OR REPLACE FUNCTION lineitem.app_user_insert(
  p_external_id VARCHAR,
  p_display_name VARCHAR
)
  RETURNS SETOF lineitem.app_user
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.app_user (external_id, display_name)
      VALUES (p_external_id, p_display_name)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;