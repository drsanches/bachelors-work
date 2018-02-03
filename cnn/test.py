from functions import array_functions
import numpy

array = array_functions.read_array_from_file("X_train_0.txt")
# print(array)

# array_functions.write_array_in_file(array, "tools\\2.txt")

image = array_functions.array_to_image(array)
# image.show()

array_copy = array_functions.image_to_array(image)
# print(array_copy)

resized_array = array_functions.resize_array(array, 50, 50)
resized_image = array_functions.array_to_image(resized_array)
# resized_image.show()

X = array_functions.prepare_for_cnn(array)
# print(X.shape)
