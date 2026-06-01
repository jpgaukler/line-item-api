CREATE FUNCTION lineitem.app_user_delete(p_id BIGINT)
  RETURNS BOOLEAN
AS
$$
DECLARE
  rows_deleted INTEGER;
BEGIN
  DELETE
  FROM lineitem.app_user
  WHERE id = p_id;

  GET DIAGNOSTICS rows_deleted = ROW_COUNT;

  RETURN rows_deleted > 0;
END;
$$ LANGUAGE plpgsql;