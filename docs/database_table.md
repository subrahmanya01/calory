# Database Schema:

## User
| Column           | Type          |
| ---------------- | ------------- |
| `id`             | UUID / BIGINT |
| `name`           | VARCHAR       |
| `email`          | VARCHAR       |
| `password_hash`  | VARCHAR       |
| `date_of_birth`  | DATE          |
| `gender`         | VARCHAR       |
| `height`         | DECIMAL       |
| `current_weight` | DECIMAL       |
| `created_at`     | TIMESTAMP     |
| `updated_at`     | TIMESTAMP     |

## Health_Goals
| Column                 | Type      |
| ---------------------- | --------- |
| `id`                   | UUID      |
| `user_id`              | UUID      |
| `daily_calorie_target` | DECIMAL   |
| `protein_target`       | DECIMAL   |
| `carb_target`          | DECIMAL   |
| `fat_target`           | DECIMAL   |
| `weight_target`        | DECIMAL   |
| `start_date`           | DATE      |
| `end_date`             | DATE      |
| `is_active`            | BOOLEAN   |
| `created_at`           | TIMESTAMP |

## Food_Entries
| Column        | Type      |
| ------------- | --------- |
| `id`          | UUID      |
| `user_id`     | UUID      |
| `meal_type`   | ENUM      |
| `food_name`   | VARCHAR   |
| `quantity`    | DECIMAL   |
| `unit`        | VARCHAR   |
| `consumed_at` | TIMESTAMP |
| `source`      | ENUM      |
| `notes`       | TEXT      |
| `created_at`  | TIMESTAMP |
| `updated_at`  | TIMESTAMP |


### meal_type (enum)
``` 
BREAKFAST
LUNCH
DINNER
SNACK
```

### source (enum)
```
MANUAL
AI
DATABASE

```

## Food_Nutrition
| Column            | Type    |
| ----------------- | ------- |
| `id`              | UUID    |
| `food_entry_id`   | UUID    |
| `calories`        | DECIMAL |
| `protein_g`       | DECIMAL |
| `carbohydrates_g` | DECIMAL |
| `fat_g`           | DECIMAL |
| `fiber_g`         | DECIMAL |
| `sugar_g`         | DECIMAL |
| `sodium_mg`       | DECIMAL |
| `calcium_mg`      | DECIMAL |
| `iron_mg`         | DECIMAL |
| `magnesium_mg`    | DECIMAL |
| `potassium_mg`    | DECIMAL |
| `zinc_mg`         | DECIMAL |
| `vitamin_a_mcg`   | DECIMAL |
| `vitamin_b1_mg`   | DECIMAL |
| `vitamin_b2_mg`   | DECIMAL |
| `vitamin_b3_mg`   | DECIMAL |
| `vitamin_b6_mg`   | DECIMAL |
| `vitamin_b12_mcg` | DECIMAL |
| `vitamin_c_mg`    | DECIMAL |
| `vitamin_d_mcg`   | DECIMAL |
| `vitamin_e_mg`    | DECIMAL |
| `vitamin_k_mcg`   | DECIMAL |
