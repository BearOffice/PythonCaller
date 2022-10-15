import sys, json

__input_limit__ = False
__output_limit__ = False
__json_output_token__ = ""

def init_environment():
    global __json_output_token__
    if __json_output_token__ == "":
        __json_output_token__ = sys.stdin.readline()

def get_input():
    if __json_output_token__ == "":
        raise Exception("CSConnector's environment has not been initialized.")

    global __input_limit__
    if __input_limit__ or __output_limit__:
        return
    __input_limit__ = True

    return json.load(sys.stdin)

# Json can only serialize or deserialize int, float, str, dict, list, tuple...
def set_output(obj):
    if __json_output_token__ == "":
        raise Exception("CSConnector's environment has not been initialized.")

    global __output_limit__
    if __output_limit__:
        return
    __output_limit__ = True

    sys.stdout.writelines([__json_output_token__])
    try:
        json.dump(obj, sys.stdout)
    except:
        if hasattr(obj, '__dict__'):  # try to convert class object to serializable object
            json.dump(obj.__dict__, sys.stdout)
        else:
            json.dump(obj, sys.stdout)