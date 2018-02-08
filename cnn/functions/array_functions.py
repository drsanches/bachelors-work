import numpy
import random
from PIL import Image
from PIL import ImageDraw

# TODO: rewrite it
def read_array_from_file(array_path):
    array = {}
    width = 0
    height = 0
    k = 0
    t = True
    for line in open(array_path, "r").read().split("\n"):
        height = height + 1
        for unit in line.split(" "):
            if unit != "":
                array[k] = float(unit)
                k = k + 1
                if t:
                    width = width + 1
            else:
                t = False
    height = height - 1
    symbol = numpy.zeros(shape=(width, height), dtype=numpy.float)
    k = 0
    for j in range(0, height):
        for i in range(0, width):
            symbol[i, j] = array[k]
            k = k + 1
    return symbol

def write_array_to_file(array, filepath):
    file = open(filepath, 'w')
    width = array.shape[0]
    height = array.shape[1]
    for j in range(0, height):
        for i in range(0, width):
            file.write(str(array[i, j]) + " ")
        file.write("\n")
    file.close()

def array_to_image(array):
    width = array.shape[0]
    height = array.shape[1]
    image = Image.new("L", (width, height))
    draw = ImageDraw.Draw(image)
    for i in range(0, width):
        for j in range(0, height):
            draw.point((i, j), int(array[i, j] * 255))
    del draw
    return image

def image_to_array(image):
    width = image.width
    height = image.height
    array = numpy.zeros(shape=(width, height), dtype=numpy.float)
    for i in range(0, width):
        for j in range(0, height):
            array[i, j] = image.getpixel((i, j)) / 255
    return array

def input_image_scaling(image, input_size, color):
    k_width = input_size[0] / image.width
    k_height = input_size[1] / image.height
    k = min(k_width, k_height)
    new_width = int(image.width * k)
    new_height = int(image.height * k)
    resized_image = image.resize((new_width, new_height), Image.ANTIALIAS)
    final_image = Image.new('L', (input_size[0], input_size[1]), color)
    x1 = int(input_size[0] / 2 - new_width / 2)
    y1 = int(input_size[1] / 2 - new_height / 2)
    x2 = int(input_size[0] / 2 + new_width / 2)
    y2 = int(input_size[1] / 2 + new_height / 2)
    final_image.paste(resized_image, (x1, y1, x2, y2))
    return final_image

def input_array_scaling(array, input_size, color):
    image = array_to_image(array)
    image = input_image_scaling(image, input_size, color)
    new_array = image_to_array(image)
    return new_array

# Because this happens when numpy saves an array
def array_diagonal_mapping(array):
    w = array.shape[0]
    h = array.shape[1]
    new_array = numpy.zeros((h, w))
    for i in range(0, w):
        for j in range(0, h):
            new_array[j, i] = array[i, j]
    return new_array

# Because this happens when numpy saves an array
def image_diagonal_mapping(image):
    array = image_to_array(image)
    array = array_diagonal_mapping(array)
    new_image = array_to_image(array)
    return new_image

def cut_symbol(image):
    x1 = image.width - 1
    y1 = image.height - 1
    x2 = 0
    y2 = 0
    for i in range(0, image.width):
        for j in range(0, image.height):
            if (image.getpixel((i, j)) < 255 and i < x1):
                x1 = i
    for j in range(0, image.height):
        for i in range(0, image.width):
            if (image.getpixel((i, j)) < 255 and j < y1):
                y1 = j
    for i in range(0, image.width):
        for j in range(0, image.height):
            i1 = image.width - 1 - i
            j1 = image.height - 1 - j
            if (image.getpixel((i1, j1)) < 255 and i1 > x2):
                x2 = i1
    for j in range(0, image.height):
        for i in range(0, image.width):
            i1 = image.width - 1 - i
            j1 = image.height - 1 - j
            if (image.getpixel((i1, j1)) < 255 and j1 > y2):
                y2 = j1
    new_image = Image.new('L', (x2 - x1 + 1, y2 - y1 + 1), "White")
    new_image.paste(image, (- x1, - y1))
    return new_image

# TODO: Fix bug (works only if image.size mod 2 == 0)
def rotate(image, angle):
    frame = (int(image.width * 0.5), int(image.height * 0.5), int(image.width * 1.5), int(image.height * 1.5))
    new_image = Image.new('L', (image.width * 2, image.height * 2), "White")
    new_image.paste(image, frame)
    new_image = new_image.rotate(angle)
    new_image = new_image.transform(image.size, Image.EXTENT, frame)
    return new_image

def random_dots(image, count):
    new_image = image.copy()
    draw = ImageDraw.Draw(new_image)
    for i in range(0, count):
        x = random.randint(0, new_image.width - 1)
        y = random.randint(0, new_image.height - 1)
        color = random.randint(0, 255)
        draw.point((x, y), fill=color)
    del draw
    return new_image

def scaling(image, size):
    scaled_image = image.copy()
    scaled_image = scaled_image.resize((int(image.width * size[0]), int(image.width * size[1])), Image.ANTIALIAS)
    new_image = Image.new('L', (image.width, image.height), "White")
    w = image.width
    h = image.height
    n_w = int(image.width * size[0])
    n_h = int(image.height * size[1])
    zero_point = (int(w / 2 - n_w / 2), int(h / 2 - n_h / 2))
    new_image.paste(scaled_image, zero_point)
    return new_image

def deformation(image, size, color):
    new_image = scaling(image, size)
    new_image = cut_symbol(new_image)
    new_image = input_image_scaling(new_image, (image.width, image.height), color)
    return new_image