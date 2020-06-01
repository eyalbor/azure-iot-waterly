import { combineReducers } from 'redux'
import { reducer as formReducer } from 'redux-form'
import authReducer from './authReducer';
import deviceReducer from './deviceReducer'
import eventReducer from './eventReducer'


export default combineReducers({
    auth: authReducer,
    form: formReducer,
    devices: deviceReducer,
    events: eventReducer
});