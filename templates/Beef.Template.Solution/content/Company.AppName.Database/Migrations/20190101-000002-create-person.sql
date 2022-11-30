-- Migration Script

START TRANSACTION;

CREATE TABLE `person` (
  `person_id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `first_name` VARCHAR(100) NULL,
  `last_name` VARCHAR(100) NULL,
  `gender_code` VARCHAR(50) NULL,
  `birthday` DATE NULL,
  `row_version` TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `created_by` VARCHAR(250) NULL,
  `created_date` DATETIME(6) NULL,
  `updated_by` VARCHAR(250) NULL,
  `updated_date` DATETIME(6) NULL
);
	
COMMIT WORK;