import { combineReducers } from 'redux'
import { reducer as formReducer } from 'redux-form'
import authReducer from './authReducer';
import deviceReducer from './deviceReducer'


export default combineReducers({
    auth: authReducer,
    form: formReducer,
    devices: deviceReducer
});