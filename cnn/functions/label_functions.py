def label_creator(cnn_out):
    cnn_out = cnn_out[0]
    result = cnn_out.argmax()
    return result
