﻿use MATRIX;

CREATE TABLE matrixdata
(
	ID int IDENTITY(0, 1) PRIMARY KEY,
	MATRIX_N int NOT NULL,
	X_POS int NOT NULL,
	Y_POS int NOT NULL,
	VALUE int NOT NULL
)