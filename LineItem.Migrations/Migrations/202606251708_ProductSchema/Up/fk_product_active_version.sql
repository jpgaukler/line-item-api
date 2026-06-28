ALTER TABLE lineitem.product
  ADD CONSTRAINT fk_product_active_version
    FOREIGN KEY (id, active_version)
      REFERENCES lineitem.product_version (product_id, version);