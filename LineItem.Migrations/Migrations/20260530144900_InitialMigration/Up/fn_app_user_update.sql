CREATE OR REPLACE FUNCTION lineitem.app_user_update(
  p_id BIGINT,
  p_external_id VARCHAR,
  p_display_name VARCHAR
)
  RETURNS SETOF lineitem.app_user
AS
$$
BEGIN
  RETURN QUERY
    UPDATE lineitem.app_user
      SET display_name = p_display_name,
        external_id = p_external_id
      WHERE id = p_id
      RETURNING *;
END;
$$ LANGUAGE plpgsql;
