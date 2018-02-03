import numpy
from keras.datasets import mnist
from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras.utils import np_utils
from keras.callbacks import TensorBoard
from functions import array_functions


# Set seed for repeatable of results
numpy.random.seed(42)

# Size of input image
img_rows = 28
img_cols = 28

# Load dataset
(X_train, y_train), (X_test, y_test) = mnist.load_data()

# Image size conversion
X_train = X_train.reshape(X_train.shape[0], img_rows, img_cols, 1)
X_test = X_test.reshape(X_test.shape[0], img_rows, img_cols, 1)
input_shape = (img_rows, img_cols, 1)

# Data normalization
X_train = X_train.astype('float32')
X_test = X_test.astype('float32')
X_train /= 255
X_test /= 255

a = numpy.copy(X_train[0])
a = a.reshape(a.shape[0], a.shape[1])
print(a.shape)
array_functions.write_array_in_file(a, "X_train_0.txt")

# Converting labels to categories
Y_train = np_utils.to_categorical(y_train, 10)
Y_test = np_utils.to_categorical(y_test, 10)

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
model.add(Dense(10, activation='softmax'))

# Compile the model
model.compile(loss="categorical_crossentropy", optimizer="adam", metrics=["accuracy"])
print(model.summary())

# Train the network
tensorboard=TensorBoard(log_dir='./logs', write_graph=True)
history = model.fit(X_train, Y_train,
                    batch_size=200,
                    epochs=3,
                    verbose=1,
                    validation_split=0.1,
                    callbacks=[tensorboard])

# Assess the quality of network training on test data
scores = model.evaluate(X_test, Y_test, verbose=1)
print("Точность работы на тестовых данных: %.2f%%" % (scores[1]*100))

# Saving
json_model = model.to_json()
json_file = open("cnn1.json", "w")
json_file.write(json_model)
json_file.close()
model.save_weights("cnn1.h5")
