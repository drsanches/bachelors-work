import numpy
from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras.callbacks import TensorBoard


dataset_filename = "..\\dataset\\dataset.npz"

# Load dataset
dataset = numpy.load(dataset_filename)
X_train = dataset["X_train"]
Y_train = dataset["Y_train"]
X_test = dataset["X_test"]
Y_test = dataset["Y_test"]

input_shape = (X_train[0].shape[0], X_train[0].shape[1], 1)
out_count = Y_train.shape[1]

# Create a sequential model
model = Sequential()
model.add(Conv2D(75, kernel_size=(5, 5), activation='relu', input_shape=input_shape))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.2))
model.add(Conv2D(100, (5, 5), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.2))
model.add(Flatten())
model.add(Dense(500, activation='relu'))
model.add(Dropout(0.5))
model.add(Dense(out_count, activation='softmax'))

# Compile the model
model.compile(loss="categorical_crossentropy", optimizer="adam", metrics=["accuracy"])
print(model.summary())

# Train the network
tensorboard=TensorBoard(log_dir='./logs', write_graph=True)
history = model.fit(X_train, Y_train,
                    batch_size=100,
                    epochs=30,
                    verbose=1,
                    validation_split=0.1,
                    callbacks=[tensorboard])

# Assess the quality of network training on test data
scores = model.evaluate(X_test, Y_test, verbose=1)
print("Точность работы на тестовых данных: %.2f%%" % (scores[1]*100))

# Saving
json_model = model.to_json()
json_file = open("cnn_data\\cnn5.json", "w")
json_file.write(json_model)
json_file.close()
model.save_weights("cnn_data\\cnn5.h5")
