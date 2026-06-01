CREATE OR REPLACE FUNCTION lineitem.app_user_retrieve_by_external_id(
  p_external_id VARCHAR
)
  RETURNS lineitem.app_user
AS
$$
DECLARE
  app_user lineitem.app_user;
BEGIN
  SELECT *
  INTO app_user
  FROM lineitem.app_user
  WHERE external_id = p_external_id;

  RETURN app_user;
END;
$$ LANGUAGE plpgsql;
