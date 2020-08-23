import {SIGN_IN, SIGN_OUT, SET_USER} from '../actions/types'

const INTIAL_STATE = {
    isSignedIn: null,
    userId: null,
    user: null
};

export default (state = INTIAL_STATE, action ) => {
    switch(action.type){
        case SIGN_IN:
            return {...state, isSignedIn:true, userId: action.payload}
        case SIGN_OUT:
            return {...state, isSignedIn:false, userId: null}
        case SET_USER:
            return {...state, user:action.payload}
        default:
            return state;
    }
};