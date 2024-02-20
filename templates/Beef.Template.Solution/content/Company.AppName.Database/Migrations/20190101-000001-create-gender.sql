﻿CREATE TABLE `gender` (
  `gender_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `code` VARCHAR(50) NOT NULL UNIQUE,
  `text` VARCHAR(250) NULL,
  `is_active` BOOL NULL,
  `sort_order` INT NULL,
  `row_version` TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `created_by` VARCHAR(250) NULL,
  `created_date` DATETIME(6) NULL,
  `updated_by` VARCHAR(250) NULL,
  `updated_date` DATETIME(6) NULL
);